import { Component, OnInit, ViewChild } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';

import { BackendService } from "../services/backend.service";

import { SearchParams } from "../interfaces/searchParams";
import { BookListComponent } from '../book-list/book-list.component';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  @ViewChild(BookListComponent) bookListComponent!: BookListComponent;

  searchParams: SearchParams = {paramsPresent: true};
  searchDateStart?: Date;
  searchDateEnd?: Date;

  constructor(private backend: BackendService) {

  }

  ngOnInit(): void {

  }

  search(): void {
    if (this.searchDateStart && this.searchDateEnd) {
      this.searchParams.searchDateStart = this.correctDate(this.searchDateStart).toISOString().substring(0,10);
      this.searchParams.searchDateEnd = this.correctDate(this.searchDateEnd).toISOString().substring(0,10);
    }
    this.bookListComponent.searchParams = this.searchParams;

    this.bookListComponent.update();
  }

  // Returns a Date matching local time.
  correctDate(input: Date): Date {
    const d = input;
    return new Date(d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes() - d.getTimezoneOffset());
  }
}