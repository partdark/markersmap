import client from './client';
import type { CategoryDto } from '../types';

export const getCategories = () => client.get<CategoryDto[]>('/categories').then((r) => r.data);

export const createCategory = (name: string) =>
  client.post<CategoryDto>('/categories', { name }).then((r) => r.data);

export const deleteCategory = (id: string) => client.delete(`/categories/${id}`);