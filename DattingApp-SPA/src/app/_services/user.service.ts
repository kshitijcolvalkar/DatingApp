import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { Message } from '../_models/message';
@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl + 'users';

  // httpOptions = {
  //   headers: new HttpHeaders({
  //     Authorization: 'Bearer ' + localStorage.getItem('token')
  //   })
  // };

  constructor(private http: HttpClient) { }

  getUsers(pageNumber?, pageSize?, userParams?, likerParams?): Observable<PaginatedResult<User[]>>
  {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

    let params = new HttpParams();
    if (pageNumber != null && pageSize != null)
    {
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    }

    if (userParams != null)
    {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likerParams === 'Likers')
    {
      params = params.append('likers', 'true');
    }

    if (likerParams === 'Likees')
    {
      params = params.append('likees', 'true');
    }

    return this.http.get<User[]>(this.baseUrl, { observe: 'response', params })
    .pipe(
      map((response) => {
        paginatedResult.result = response.body;
        if (response.headers.get('pagination'))
        {
          paginatedResult.pagination = JSON.parse(response.headers.get('pagination'));
        }
        return paginatedResult;
      })
    );
  }

  getUser(id: number): Observable<User>
  {
    return this.http.get<User>(this.baseUrl + '/' + id);
  }

  updateUser(id: number, user: User)
  {
    return this.http.put(this.baseUrl + '/' + id, user);
  }

  setMainPhoto(userId: number, id: number)
  {
    return this.http.post(this.baseUrl + '/' + userId + '/photos/' + id + '/setmain', {});
  }

  deletePhoto(userId: number, id: number)
  {
    return this.http.delete(this.baseUrl + '/' + userId + '/photos/' + id);
  }

  addLike(userId: number, recepientId: number)
  {
    return this.http.post(this.baseUrl + '/' + userId + '/like/' + recepientId, {});
  }

  getMessages(userId: number, pageNumber?, pageSize?, messageContainer?)
  {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();

    let params = new HttpParams();
    params = params.append('messageContainer', messageContainer);
    if (pageNumber != null && pageSize != null)
    {
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    }

    return this.http.get<Message[]>(this.baseUrl + '/' + userId + '/messages', { observe: 'response', params})
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null)
          {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
  }

  getMessageThread(userId: number, recepientId: number)
  {
    return this.http.get<Message[]>(this.baseUrl + '/' + userId + '/messages/thread/' + recepientId);
  }

  sendMessage(userId: number, message: Message)
  {
    return this.http.post<Message>(this.baseUrl + '/' + userId + '/messages', message);
  }

  deleteMessage(userId: number, id: number)
  {
    return this.http.post(this.baseUrl + '/' + userId + '/messages/' + id, {});
  }

  markAsRead(userId: number, id: number)
  {
    this.http.post(this.baseUrl + '/' + userId + '/messages/' + id + '/read', {}).subscribe();
  }
}
