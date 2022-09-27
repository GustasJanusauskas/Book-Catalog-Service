import { Component, OnInit, ViewChild } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Observable, forkJoin } from 'rxjs';
import { Router } from '@angular/router';

import { HelperFunctionsService } from "../services/helper-functions.service";
import { BackendService } from "../services/backend.service";

import { BookStatus } from "../interfaces/bookStatus";
import { Book } from "../interfaces/book";

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit {
  @ViewChild('listings') listings: any;

  borrowedList: BookStatus[] = [];

  paginatorLength: number = 0;
  pageEvent?: PageEvent;

  constructor(private backend: BackendService, private router: Router) {

  }

  pageChange(event: PageEvent) {
    this.pageEvent = event;
    this.borrowedList = [];
    this.updateList(event.pageIndex * event.pageSize);
  }

  updateList(offset: number = 0): void {
    this.backend.getAdminListings(HelperFunctionsService.getCookie('session') || '',offset).subscribe(data => {
      this.listings.deselectAll();

      this.paginatorLength = data.total;
      this.borrowedList = data.listings;
    });
  }

  ngOnInit(): void {
    //Kick any unprivileged users out, if a user adds the cookie manually, all of the API calls will reject and return empty anyway.
    if (HelperFunctionsService.getCookie('privilege') != 'admin') {
      this.router.navigate(['/']);
    }

    this.updateList();
  }

  changeListingStatus(status: string = 'borrowed_by'): void {
    var observables: Observable<any>[] = [];

    //Send requests for each selected listing
    const selectedLength = this.listings.selectedOptions.selected.length;
    for (let i = 0; i < selectedLength; i++) {
      const el = this.listings.selectedOptions.selected[i];
      observables.push(this.backend.changeBookStatusAdmin(HelperFunctionsService.getCookie('session') || '',el.value,status,false));
    }
    //Update list when all of the requests have finished
    forkJoin(observables).subscribe( () => {
      this.updateList( (this.pageEvent?.pageIndex || 0) * 15);
    });
  }
}