import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HeaderComponent } from "../../../../shared/components/header/header.component";
import { FooterComponent } from "../../../../shared/components/footer/footer.component";

@Component({
  selector: 'app-email-confirmed-page',
  imports: [CommonModule, HeaderComponent, FooterComponent, RouterLink],
  templateUrl: './email-confirmed-page.component.html',
  styleUrl: './email-confirmed-page.component.scss',
})
export class EmailConfirmedPageComponent implements OnInit {
  success = false;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.success = params['success'] === 'true';
    });
  }
}