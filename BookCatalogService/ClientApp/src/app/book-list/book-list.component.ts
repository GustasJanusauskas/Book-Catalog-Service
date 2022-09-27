import { Component, OnInit, Input } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';

import { BackendService } from "../services/backend.service";

import { BookStatus } from '../interfaces/bookStatus';
import { SearchParams } from '../interfaces/searchParams';
import { HelperFunctionsService } from '../services/helper-functions.service';

export const TILE_COLOURS_1: string[] = ['#006064','#00838f','#0098a7','#00adc1','#00bdd4','#26c7da','#4dd1e1','#80dfea','#b2ebf2'];
export const TILE_COLOURS_2: string[] = ['#1a237e','#283593','#303f9f','#3949ab','#3f51b5','#5c6bc0','#7986cb','#9fa8da','#c5cae9'];

@Component({
  selector: 'app-book-list',
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.css']
})
export class BookListComponent implements OnInit {
    public searchParams: SearchParams = {paramsPresent: false};
    public bookList: BookStatus[] = []; 

    paginatorLength: number = 0;

    constructor(private backend: BackendService) {

    }

    setColours() {
        this.bookList.forEach(listing => {
            listing.colour = `linear-gradient(45deg,${ TILE_COLOURS_1[HelperFunctionsService.seededRandomInt(listing.id,TILE_COLOURS_1.length - 1)] } 0%,${ TILE_COLOURS_2[HelperFunctionsService.seededRandomInt(listing.id + 1,TILE_COLOURS_2.length - 1)] } 100%);`;
        });
    }

    pageChange(event: PageEvent) {
        this.bookList = [];
        this.backend.getListings(event.pageIndex * event.pageSize,this.searchParams).subscribe( data => {
            this.bookList = data.listings;
            this.setColours();
        });
    }

    update(callback?: Function) {
        this.bookList = [];
        this.backend.getListings(0,this.searchParams).subscribe( data => {
            this.paginatorLength = data.total;
            this.bookList = data.listings;
            this.setColours();

            if (callback) callback();
        });
    }

    ngOnInit() {
        this.backend.getListings().subscribe( data => {
            this.paginatorLength = data.total;
            this.bookList = data.listings;
            this.setColours();
        });
    }
}