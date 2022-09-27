import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { BackendService } from "../services/backend.service";
import { HelperFunctionsService } from "../services/helper-functions.service";

import { Book } from '../interfaces/book';
import { BookStatus } from '../interfaces/bookStatus';
import { TILE_COLOURS_1, TILE_COLOURS_2} from '../book-list/book-list.component';

@Component({
  selector: 'app-view-book',
  templateUrl: './view-book.component.html',
  styleUrls: ['./view-book.component.css']
})
export class ViewBookComponent implements OnInit {
  listing: BookStatus = {id:-1};
  session: string = "";
  userID: number = -1;

  disableReserve: boolean = false;
  disableBorrow: boolean = false;

  constructor(private route: ActivatedRoute, private backend:BackendService,private snackBar: MatSnackBar) { 

  }

  setColour(): void {
    this.listing.colour = `linear-gradient(45deg,${ TILE_COLOURS_1[HelperFunctionsService.seededRandomInt(this.listing.id,TILE_COLOURS_1.length - 1)] } 0%,${ TILE_COLOURS_2[HelperFunctionsService.seededRandomInt(this.listing.id + 1,TILE_COLOURS_2.length - 1)] } 100%);`;
  }
  
  ngOnInit(): void {
    //Get user session
    this.session = HelperFunctionsService.getCookie("session") || "";
    if (this.session != "") this.backend.getUserInfo(this.session).subscribe( data => {
      this.userID = data.userid;
      this.getListingInfo();
    });
    else this.getListingInfo();
  }

  getListingInfo(): void {
    //Get book info from server
    this.route.queryParams.subscribe( params => {
      this.backend.getListingInfo(params.id).subscribe( data => {
        this.listing = data;
        this.disableReserve = !(data.reserved_by == -1) || data.borrowed_by == this.userID;
        this.disableBorrow = !(data.borrowed_by == -1);
        this.setColour();
      });
    });
  }

  reserveBook(status: boolean = true): void {
    this.backend.changeBookStatus(this.session,this.listing.id,"reserved_by",status).subscribe();

    if (!status) this.listing.reserved_by = -1;
    else this.listing.reserved_by = this.userID;
    
    this.disableBorrow = true;
    this.disableReserve = false;
    this.snackBar.open('Changed book reservation status successfully!', undefined, { duration: 5000});
  }

  borrowBook(): void {
    this.backend.changeBookStatus(this.session,this.listing.id,"borrowed_by",true).subscribe();
    this.disableBorrow = true;
    this.disableReserve = true;
    this.snackBar.open('Borrowed book successfully!', undefined, { duration: 5000});
  }
}