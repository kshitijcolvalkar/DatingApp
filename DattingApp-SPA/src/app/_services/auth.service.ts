import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl: any = 'http://localhost:5000/api/user/';
  constructor(private http: HttpClient) { }

  login(model: any)
  {
    return this.http.post(this.baseUrl + 'login', model)
    .pipe(
      map((resposne: any) => {
        const user = resposne;

        if (user)
        {
          localStorage.setItem('token', user.token);
        }
      })
    );
  }

  register(model: any)
  {
    return this.http.post(this.baseUrl + 'register', model);
  }
}
