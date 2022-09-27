import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';

import { BackendService } from "../services/backend.service";
import { HelperFunctionsService } from "../services/helper-functions.service";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  username = new FormControl("", [Validators.minLength(8),Validators.required] );
  password = new FormControl("", [Validators.minLength(8),Validators.required] );

  constructor(private backEnd: BackendService,private snackBar: MatSnackBar) { }

  ngOnInit(): void {
  }

  getErrorMsg(form: FormControl): string {
    if (form.hasError('required')) return 'You must enter a value.';
    else if (form.hasError('minlength')) return 'Value must be atleast 8 characters long.';

    return '';
  }

  login(): void {
    if (this.username.invalid || this.password.invalid) return;

    this.backEnd.loginUser(this.username.value!,this.password.value!).subscribe( data => {
      if (data.error) {
        this.snackBar.open(data.error, undefined, { duration: 5000});
        return;
      }

      if (data.privileged) HelperFunctionsService.setCookie('privilege','admin'); //Is only used to show admin tab in nav-menu, privilege is otherwise verified on server.
      HelperFunctionsService.setCookie('session',data.session);
      this.snackBar.open('Logged in successfully!', undefined, { duration: 5000});
      
      this.username.markAsUntouched();
      this.password.markAsUntouched();
      this.username.setValue('');
      this.password.setValue('');
    });
  }

}