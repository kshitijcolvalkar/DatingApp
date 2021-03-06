import { Component, OnInit } from '@angular/core';
import { AuthService } from './_services/auth.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'DattingApp-SPA';
  jwtHelper = new JwtHelperService();
  constructor(private authService: AuthService){}

  ngOnInit()
  {
    const token = localStorage.getItem('token');
    const currentUser: User = JSON.parse(localStorage.getItem('user'));
    if (token)
    {
      this.authService.decodedToken = this.jwtHelper.decodeToken(token);
      this.authService.currentUser = currentUser;
      this.authService.changeMemberPhoto(currentUser.photoUrl);
    }
  }
}
