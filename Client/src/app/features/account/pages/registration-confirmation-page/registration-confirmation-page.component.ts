import { Component } from '@angular/core';
import { HeaderComponent } from "../../../../shared/components/header/header.component";
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../../../shared/components/footer/footer.component";

@Component({
  selector: 'app-registration-confirmation-page',
  imports: [HeaderComponent, RouterLink, FooterComponent],
  templateUrl: './registration-confirmation-page.component.html',
  styleUrl: './registration-confirmation-page.component.scss'
})
export class RegistrationConfirmationPageComponent {

}
