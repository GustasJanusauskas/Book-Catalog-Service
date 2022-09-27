import { Injectable } from '@angular/core';
import { HttpClient, HttpHandler, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

import { BookStatus } from '../interfaces/bookStatus';
import { SearchParams } from "../interfaces/searchParams";

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type':  'application/json'
  })
};

@Injectable({
  providedIn: 'root'
})
export class BackendService {

  constructor(private http: HttpClient) {}

  //Users
  registerUser(user: string, pass: string, email:string): Observable<any> {
    var data = {user, pass, email};
    return this.http.post<any>('/api/register',data,httpOptions);
  }

  loginUser(user: string, pass: string): Observable<any> {
    var data = {user, pass};
    return this.http.post<any>('/api/login',data,httpOptions);
  }

  getListingInfo(listingID: number): Observable<BookStatus> {
    var data = {listingID};
    return this.http.post<BookStatus>('/api/listing',data,httpOptions);
  }

  getListings(firstRecord: number = 0, searchParams: SearchParams = {paramsPresent: false}): Observable<any> {
    var data = {firstRecord,searchParams};
    return this.http.post<any>('/api/listings',data,httpOptions);
  }

  getStats(): Observable<any> {
    return this.http.get<any>('/api/stats',httpOptions);
  }

  changeBookStatus(session: string, bookid: number, column: string,state: boolean): Observable<any> {
    var data = {session,bookid,column,state};
    return this.http.post<any>('/api/bookstatuschange',data,httpOptions);
  }

  getUserInfo(session: string): Observable<any> {
    var data = {session};
    return this.http.post<any>('/api/userinfo',data,httpOptions);
  }

  //Admins
  getAdminListings(userSession:string ,firstRecord: number = 0): Observable<any> {
    var data = {userSession,firstRecord};
    return this.http.post<any>('/api/adminlistings',data,httpOptions);
  }

  changeBookStatusAdmin(session: string, bookid: number, column: string,state: boolean): Observable<any> {
    var data = {session,bookid,column,state};
    return this.http.post<any>('/api/adminbookstatuschange',data,httpOptions);
  }
}