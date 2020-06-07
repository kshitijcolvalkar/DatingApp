import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { AuthService } from '../_services/auth.service';
import { UserService } from '../_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';
  constructor(private authService: AuthService, private userService: UserService, 
              private route: ActivatedRoute, private alertify: AlertifyService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    });
  }

  getMessages()
  {
    this.userService.getMessages(+this.authService.decodedToken.nameid, this.pagination.pageNumber,
         this.pagination.pageSize, this.messageContainer)
         .subscribe((data: PaginatedResult<Message[]>) => {
            this.messages = data.result;
            this.pagination = data.pagination;
         }, error => {
           this.alertify.error(error);
         });
  }

  pageChanged(event: any): void{
    this.pagination.pageNumber = event.page;
    this.getMessages();
  }

  deleteMessage(id: number)
  {
    this.alertify.confirm('Are you sure you want to delete this message?', () => {
      this.userService.deleteMessage(+this.authService.decodedToken.nameid, id).subscribe(() => {
        this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
        this.alertify.success('Message deleted successfully');
      }, error => {
        this.alertify.error(error);
      });
    });
  }

}
