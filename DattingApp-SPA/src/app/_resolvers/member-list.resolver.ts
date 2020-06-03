import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';
import { PaginatedResult } from '../_models/pagination';

@Injectable()
export class MemberListResolver implements Resolve<PaginatedResult<User[]>>
{
    pageNumber = 1;
    pageSize = 5;
    constructor(private userService: UserService,
                private router: Router,
                private alertify: AlertifyService){}

    resolve(activeRoute: ActivatedRouteSnapshot): Observable<PaginatedResult<User[]>>
    {
        return this.userService.getUsers(this.pageNumber, this.pageSize, null).pipe(
                catchError((error) =>
                {
                    this.alertify.error(error);
                    this.router.navigate(['/home']);
                    return of(null);
                })
            );
    }
}
