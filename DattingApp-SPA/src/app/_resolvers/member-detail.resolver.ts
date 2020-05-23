import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { catchError } from 'rxjs/operators';

@Injectable()
export class MemberDetailResolver implements Resolve<Observable<User>>
{
    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService){}

    resolve(activeRoute: ActivatedRouteSnapshot): Observable<User>
    {
        return this.userService.getUser(+activeRoute.params['id']).pipe(
                catchError((error) =>
                {
                    this.alertify.error(error);
                    this.router.navigate(['/matches']);
                    return of(null);
                })
            );
    }
}
