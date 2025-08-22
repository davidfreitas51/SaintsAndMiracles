import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { HeaderComponent } from '../../../../shared/components/header/header.component';
import { RegisterDto } from '../../interfaces/register-dto';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    FooterComponent,
    HeaderComponent,
    RouterLink
  ],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss',
})
export class RegisterPageComponent {
  registerDto: RegisterDto = {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    inviteToken: '',
  };

  onSubmit() {
    if (this.registerDto.password !== this.registerDto.confirmPassword) {
      alert('Passwords do not match!');
      return;
    }

    console.log('Registering user:', this.registerDto);
  }
}
