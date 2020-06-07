import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {

  @Input() recepientId: number;
  messages: Message[];
  newMessage: any = {};
  constructor(private userService: UserService, private authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.getMessageThread();
  }

  getMessageThread()
  {
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(+this.authService.decodedToken.nameid, this.recepientId)
    .pipe(
      tap((messages: Message[]) => {
        // tslint:disable-next-line: prefer-for-of
        for (let i = 0; i < messages.length; i++) {
          if (messages[i].isRead === false && messages[i].recepientId === currentUserId)
          {
            this.userService.markAsRead(currentUserId, messages[i].id);
          }
        }
      })
    )
    .subscribe((messages: Message[]) => {
      this.messages = messages;
    }, error => {
      this.alertify.error(error);
    });
  }

  sendMessage()
  {
    this.newMessage.recepientId = this.recepientId;
    this.userService.sendMessage(+this.authService.decodedToken.nameid, this.newMessage).subscribe((message: Message) => {
      this.messages.unshift(message);
      this.newMessage.content = '';
    }, error => {
      this.alertify.error(error);
    });
  }

}
