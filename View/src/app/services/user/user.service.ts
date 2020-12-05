import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from "rxjs";
import {User} from "../../models/user";

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private httpClient: HttpClient) { }

  getUsers(): Observable<User[]>{
    return this.httpClient.get<User[]>('http://localhost:5000/api/users');
  }

  getModeration(): Observable<User[]>{
    return this.httpClient.get<User[]>('http://localhost:5000/api/users/moderation');
  }

  register(user: User): Observable<User[]>{
    return this.httpClient.post<User[]>('/api/users/register', user);
  }

  login(user: User): Observable<User[]>{
    return this.httpClient.post<User[]>('/api/users/login', user);
  }
}