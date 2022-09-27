import { Book } from './book';

export interface BookStatus {
    id: number;
    book?: Book;

    borrowed_by?: number;
    reserved_by?: number;
    colour?: string;
}