import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { CatalogService } from '../catalog.service';
import { SalesService } from '../sales.service';
import { CartLine, Category, CreateSale, PaymentMethod, Product, Sale } from '../models';

@Component({
  selector: 'app-pos',
  imports: [FormsModule, DecimalPipe],
  templateUrl: './pos.html',
  styleUrl: './pos.scss'
})
export class Pos implements OnInit {
  private readonly catalog = inject(CatalogService);
  private readonly sales = inject(SalesService);

  readonly products = signal<Product[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly cart = signal<CartLine[]>([]);

  readonly search = signal('');
  readonly selectedCategory = signal<number | null>(null);

  readonly cashierName = signal('');
  readonly paymentMethod = signal<PaymentMethod>('Cash');
  readonly amountTendered = signal<number | null>(null);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly lastSale = signal<Sale | null>(null);

  // Money math mirrored client-side for instant feedback; the server remains authoritative.
  readonly subtotal = computed(() =>
    this.round(this.cart().reduce((sum, l) => sum + l.product.unitPrice * l.quantity, 0)));

  readonly taxTotal = computed(() =>
    this.round(this.cart().reduce((sum, l) => sum + l.product.unitPrice * l.quantity * l.product.taxRate, 0)));

  readonly grandTotal = computed(() => this.round(this.subtotal() + this.taxTotal()));

  readonly changeDue = computed(() => {
    const tendered = this.amountTendered() ?? 0;
    return this.round(Math.max(0, tendered - this.grandTotal()));
  });

  readonly canCheckout = computed(() => {
    if (this.cart().length === 0 || this.loading()) {
      return false;
    }
    if (this.paymentMethod() === 'Cash') {
      return (this.amountTendered() ?? 0) >= this.grandTotal();
    }
    return true;
  });

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.catalog.getCategories().subscribe({
      next: (c) => this.categories.set(c),
      error: () => this.error.set('Could not load categories.')
    });
  }

  loadProducts(): void {
    this.loading.set(true);
    this.catalog.getProducts({
      search: this.search() || undefined,
      categoryId: this.selectedCategory() ?? undefined,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load products. Is the Catalog service running?');
        this.loading.set(false);
      }
    });
  }

  selectCategory(categoryId: number | null): void {
    this.selectedCategory.set(categoryId);
    this.loadProducts();
  }

  addToCart(product: Product): void {
    this.error.set(null);
    const lines = [...this.cart()];
    const existing = lines.find((l) => l.product.id === product.id);
    const inCart = existing?.quantity ?? 0;
    if (inCart + 1 > product.stockQuantity) {
      this.error.set(`Only ${product.stockQuantity} of "${product.name}" in stock.`);
      return;
    }
    if (existing) {
      existing.quantity += 1;
    } else {
      lines.push({ product, quantity: 1 });
    }
    this.cart.set(lines);
  }

  changeQuantity(line: CartLine, delta: number): void {
    const next = line.quantity + delta;
    if (next <= 0) {
      this.removeLine(line);
      return;
    }
    if (next > line.product.stockQuantity) {
      this.error.set(`Only ${line.product.stockQuantity} of "${line.product.name}" in stock.`);
      return;
    }
    this.cart.set(this.cart().map((l) => (l.product.id === line.product.id ? { ...l, quantity: next } : l)));
  }

  removeLine(line: CartLine): void {
    this.cart.set(this.cart().filter((l) => l.product.id !== line.product.id));
  }

  clearCart(): void {
    this.cart.set([]);
    this.amountTendered.set(null);
    this.error.set(null);
  }

  checkout(): void {
    if (!this.canCheckout()) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const payload: CreateSale = {
      items: this.cart().map((l) => ({ productId: l.product.id, quantity: l.quantity })),
      cashierName: this.cashierName() || undefined,
      paymentMethod: this.paymentMethod(),
      amountTendered: this.paymentMethod() === 'Cash' ? this.amountTendered() ?? 0 : this.grandTotal()
    };

    this.sales.createSale(payload).subscribe({
      next: (sale) => {
        this.lastSale.set(sale);
        this.cart.set([]);
        this.amountTendered.set(null);
        this.loading.set(false);
        this.loadProducts(); // refresh stock levels after the sale
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'Checkout failed. Please try again.');
        this.loading.set(false);
      }
    });
  }

  dismissReceipt(): void {
    this.lastSale.set(null);
  }

  private round(value: number): number {
    return Math.round((value + Number.EPSILON) * 100) / 100;
  }
}
