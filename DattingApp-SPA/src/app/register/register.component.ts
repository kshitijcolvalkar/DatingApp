import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker/public_api';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Output() cancelRegister = new EventEmitter();
  user: User;
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;
  constructor(private authService: AuthService, private alertify: AlertifyService,
              private formBuilder: FormBuilder, private router: Router) { }

  ngOnInit() {
    // this.registerForm = new FormGroup(
    //   {
    //     username: new FormControl('', Validators.required),
    //     password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
    //     confirmPassword: new FormControl('', Validators.required)
    //   }, this.validatePassword
    // );

    this.createRegisterForm();
    this.bsConfig = {
      containerClass: 'theme-red',
      dateInputFormat: 'DD-MM-YYYY',
    };
  }

  createRegisterForm()
  {
    this.registerForm = this.formBuilder.group(
      {
        gender: ['male'],
        username: ['', Validators.required],
        knownAs: ['', Validators.required],
        dateOfBirth: [null, Validators.required],
        city: ['', Validators.required],
        country: ['', Validators.required],
        password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
        confirmPassword: ['', Validators.required]
      }, { validator: this.validatePassword}
    );
  }

  validatePassword(g: FormGroup)
  {
    return g.get('password').value === g.get('confirmPassword').value ? null : { mismatch : true};
  }

  register()
  {
    if(this.registerForm.valid)
    {
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
        this.alertify.success('User registered successfully');
      }, (error) => {
        this.alertify.error(error);
      }, () => {
        this.authService.login(this.user).subscribe(() => {
          this.router.navigate(['/matches']);
        });
      });
    }
  }

  cancel()
  {
    this.cancelRegister.emit(false);
    this.alertify.warning('cancelled');
  }

}
