import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HeaderComponent } from "../../../../shared/components/header/header.component";
import { FooterComponent } from "../../../../shared/components/footer/footer.component";
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-email-confirmed-page',
  imports: [HeaderComponent, FooterComponent, RouterLink, MatCardModule, MatButtonModule],
  templateUrl: './email-confirmed-page.component.html',
  styleUrl: './email-confirmed-page.component.scss',
})
export class EmailConfirmedPageComponent implements OnInit {
  success = false;
  logout = false;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.success = params['success'] === 'true';
      this.logout = params['logout'] === 'true';
    });
  }
}
