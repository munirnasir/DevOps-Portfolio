import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { CatalogService, ProductWriteModel } from '../catalog.service';
import { Category, Product } from '../models';

interface ProductDraft extends ProductWriteModel {
  id?: string;
}

@Component({
  selector: 'app-products-admin',
  imports: [FormsModule, DecimalPipe],
  templateUrl: './products-admin.html',
  styleUrl: './products-admin.scss'
})
export class ProductsAdmin implements OnInit {
  private readonly catalog = inject(CatalogService);

  readonly products = signal<Product[]>([]);
  readonly categories = signal<Category[]>([]);

  readonly search = signal('');
  readonly includeInactive = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  // The product currently being created/edited (null = panel closed).
  readonly draft = signal<ProductDraft | null>(null);
  readonly isNew = computed(() => this.draft() !== null && !this.draft()!.id);

  // Inline stock-adjustment input value, keyed by product id.
  readonly stockDelta = signal<Record<string, number>>({});

  ngOnInit(): void {
    this.catalog.getCategories().subscribe({
      next: (c) => this.categories.set(c),
      error: () => this.error.set('Could not load categories.')
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.catalog.getProducts({
      search: this.search() || undefined,
      includeInactive: this.includeInactive(),
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load products.');
        this.loading.set(false);
      }
    });
  }

  runSearch(): void {
    this.page.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.page.set(page);
    this.load();
  }

  categoryName(id: number): string {
    return this.categories().find((c) => c.id === id)?.name ?? '';
  }

  newProduct(): void {
    const firstCategory = this.categories()[0]?.id ?? 1;
    this.draft.set({
      name: '', sku: '', barcode: '', description: '',
      categoryId: firstCategory, unitPrice: 0, taxRate: 0, stockQuantity: 0
    });
  }

  edit(product: Product): void {
    this.draft.set({
      id: product.id,
      name: product.name,
      description: product.description,
      categoryId: product.categoryId,
      unitPrice: product.unitPrice,
      taxRate: product.taxRate,
      isActive: product.isActive
    });
  }

  cancelEdit(): void {
    this.draft.set(null);
  }

  patchDraft(patch: Partial<ProductDraft>): void {
    const current = this.draft();
    if (current) {
      this.draft.set({ ...current, ...patch });
    }
  }

  save(): void {
    const draft = this.draft();
    if (!draft || !draft.name.trim()) {
      return;
    }
    this.loading.set(true);
    this.error.set(null);

    const request$ = draft.id
      ? this.catalog.updateProduct(draft.id, draft)
      : this.catalog.createProduct(draft);

    request$.subscribe({
      next: () => {
        this.draft.set(null);
        this.load();
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? err?.error?.message ?? 'Save failed.');
        this.loading.set(false);
      }
    });
  }

  setStockDelta(id: string, value: number): void {
    this.stockDelta.set({ ...this.stockDelta(), [id]: value });
  }

  applyStock(product: Product): void {
    const change = this.stockDelta()[product.id] ?? 0;
    if (!change) {
      return;
    }
    this.error.set(null);
    this.catalog.adjustStock(product.id, change, 'Manual adjustment').subscribe({
      next: () => {
        this.setStockDelta(product.id, 0);
        this.load();
      },
      error: (err) => this.error.set(err?.error?.detail ?? 'Stock adjustment failed.')
    });
  }

  deactivate(product: Product): void {
    this.catalog.deleteProduct(product.id).subscribe({
      next: () => this.load(),
      error: () => this.error.set('Could not deactivate product.')
    });
  }
}
