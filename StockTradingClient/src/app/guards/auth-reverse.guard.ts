import { CanActivateFn } from '@angular/router';
import { Router } from '@angular/router';

export const authReverseGuard: CanActivateFn = (route, state) => {
  const router = new Router();

  const token = sessionStorage.getItem('authToken');

  if (token) {
    router.navigate(['/dashboard']);
    return false;
  } else {
    return true;
  }
};
