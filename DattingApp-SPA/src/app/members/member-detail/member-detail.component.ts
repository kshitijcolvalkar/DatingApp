import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/_models/user';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery-9';
import { TabsetComponent } from 'ngx-bootstrap/tabs/public_api';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  @ViewChild('memberTab', { static: true}) memberTab: TabsetComponent;
  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe((data) => {
      this.user = data['user'];
    });
    console.log(this.route.queryParams);
    this.route.queryParams.subscribe(queryParams => {
      const selectedTab = queryParams['tab'];
      this.memberTab.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
    });

    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }];

    this.galleryImages = this.getImages();
  }

  getImages()
  {
    const imageUrls = [];
    for (const photo of this.user.photos) {
      imageUrls.push({
        big: photo.url,
        small: photo.url,
        medium: photo.url,
        description: photo.description
      });
    }
    return imageUrls;
  }

  selectTab(tabId: number)
  {
    this.memberTab.tabs[tabId].active = true;
  }

  // getUser()
  // {
  //   this.userService.getUser(+this.route.snapshot.params['id']).subscribe((user: User) => {
  //     this.user = user;
  //   }, error => {
  //     this.alertify.error(error);
  //   });
  // }

}
