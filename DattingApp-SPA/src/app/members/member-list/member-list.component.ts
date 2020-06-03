import { Component, OnInit } from '@angular/core';
import { PaginatedResult, Pagination } from 'src/app/_models/pagination';
import { User } from '../../_models/user';
import { AlertifyService } from '../../_services/alertify.service';
import { UserService } from '../../_services/user.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  users: User[];
  pagination: Pagination;
  user: User = JSON.parse(localStorage.getItem('user'));
  genderLists = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}];
  userParams: any = {};
  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute) { }
  ngOnInit() {
    this.route.data.subscribe((data) => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });

    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }

  getUsers()
  {
    this.userService.getUsers(this.pagination.pageNumber, this.pagination.pageSize, this.userParams)
    .subscribe((users: PaginatedResult<User[]>) => {
      this.users = users.result;
      this.pagination = users.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }

  resetFilters()
  {
    this.pagination.pageNumber = 1;
    this.pagination.pageSize = 5;
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.getUsers();
  }

  pageChanged(event: any): void {
    this.pagination.pageNumber = event.page;
    this.getUsers();
  }

}
