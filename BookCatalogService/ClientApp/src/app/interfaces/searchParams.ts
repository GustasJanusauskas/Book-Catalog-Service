export interface SearchParams {
    paramsPresent: boolean;

    searchTitle?: string;
    searchAuthor?: string;
    searchPublisher?: string;
    searchGenre?: string;
    searchISBN?: string;

    searchDateStart?: string;
    searchDateEnd?: string;

    canBorrow?: boolean;
    canReserve?: boolean;
}