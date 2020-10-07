import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map} from 'rxjs/operators';
import {JwtHelperService} from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

constructor(private http: HttpClient) { }
    baseUrl = environment.rootUrl + 'auth/';
    jwtHelper = new JwtHelperService();
    decodedToken: any;
    currentUser: User;
    photoUrl = new BehaviorSubject<string>('../../assets/original.png');
    currentPhotoUrl = this.photoUrl.asObservable();

login(model: any) {
  return this.http.post(this.baseUrl + 'login', model)
  .pipe(
    map((response: any) => {
      const user = response;
      if (user) {
        localStorage.setItem('token', user.token);
        localStorage.setItem('user', JSON.stringify(user.user));
        this.decodedToken = this.jwtHelper.decodeToken(user.token);
        this.currentUser = user.user;
      }
    })
  );
}
changeMemberPhoto(photoUrl: string) {
  this.photoUrl.next(photoUrl);
}
register(user: User) {
  return this.http.post(this.baseUrl + 'register', user);
}
loggedIn() {
  const token = localStorage.getItem('token');
  return !this.jwtHelper.isTokenExpired(token);
}
roleMatch(allowedRoles): boolean {
  let isMatch = false;
  const userRoles = this.decodedToken.role as Array<string>;
  allowedRoles.foreach(element => {
    if (userRoles.includes(element)) {
      isMatch = true;
      return;
    }
  });
  return isMatch;
}
}
