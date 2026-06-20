import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, PagedResult, Product } from './models';

export interface ProductQuery {
  search?: string;
  categoryId?: number;
  includeInactive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ProductWriteModel {
  sku?: string;
  barcode?: string;
  name: string;
  description?: string;
  categoryId: number;
  unitPrice: number;
  taxRate: number;
  stockQuantity?: number;
  isActive?: boolean;
}

// All requests use relative /api paths. In dev the Angular proxy forwards them to the
// services; in containers the nginx gateway routes them to the owning microservice.
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);

  getProducts(query: ProductQuery = {}): Observable<PagedResult<Product>> {
    const params: Record<string, string> = {};
    if (query.search) params['search'] = query.search;
    if (query.categoryId) params['categoryId'] = String(query.categoryId);
    if (query.includeInactive) params['includeInactive'] = 'true';
    if (query.page) params['page'] = String(query.page);
    if (query.pageSize) params['pageSize'] = String(query.pageSize);
    return this.http.get<PagedResult<Product>>('/api/products', { params });
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>('/api/categories');
  }

  createProduct(model: ProductWriteModel): Observable<Product> {
    return this.http.post<Product>('/api/products', model);
  }

  updateProduct(id: string, model: ProductWriteModel): Observable<Product> {
    return this.http.put<Product>(`/api/products/${id}`, model);
  }

  adjustStock(id: string, quantityChange: number, reason?: string): Observable<Product> {
    return this.http.post<Product>(`/api/products/${id}/stock-adjustment`, { quantityChange, reason });
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`/api/products/${id}`);
  }
}
