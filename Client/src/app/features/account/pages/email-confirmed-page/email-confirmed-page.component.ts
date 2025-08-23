import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-email-confirmed-page',
  imports: [CommonModule],
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