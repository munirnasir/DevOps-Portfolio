// Shared API contracts for the POS terminal. The fields mirror the DTOs returned by
// the Catalog and Sales microservices.

export interface Product {
  id: string;
  sku: string;
  barcode?: string;
  name: string;
  description?: string;
  categoryId: number;
  categoryName: string;
  unitPrice: number;
  taxRate: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
}

export type PaymentMethod = 'Cash' | 'Card';

export interface CreateSaleItem {
  productId: string;
  quantity: number;
}

export interface CreateSale {
  items: CreateSaleItem[];
  cashierName?: string;
  paymentMethod: PaymentMethod;
  amountTendered: number;
}

export interface SaleItem {
  productId: string;
  sku: string;
  name: string;
  unitPrice: number;
  taxRate: number;
  quantity: number;
  lineSubtotal: number;
  lineTax: number;
  lineTotal: number;
}

export interface Sale {
  id: string;
  number: number;
  cashierName?: string;
  createdAtUtc: string;
  subtotal: number;
  taxTotal: number;
  grandTotal: number;
  paymentMethod: PaymentMethod;
  amountTendered: number;
  changeDue: number;
  items: SaleItem[];
}

// A line in the on-screen cart before the sale is rung up.
export interface CartLine {
  product: Product;
  quantity: number;
}
