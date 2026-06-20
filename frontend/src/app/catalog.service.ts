import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, Product } from './models';

// All requests use relative /api paths. In dev the Angular proxy forwards them to the
// services; in containers the nginx gateway routes them to the owning microservice.
@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);

  getProducts(search?: string, categoryId?: number): Observable<Product[]> {
    const params: Record<string, string> = {};
    if (search) {
      params['search'] = search;
    }
    if (categoryId) {
      params['categoryId'] = String(categoryId);
    }
    return this.http.get<Product[]>('/api/products', { params });
  }

  getByBarcode(barcode: string): Observable<Product> {
    return this.http.get<Product>(`/api/products/barcode/${encodeURIComponent(barcode)}`);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>('/api/categories');
  }
}
