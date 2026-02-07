import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminContentTableComponent } from '../../../../shared/components/admin-content-table/admin-content-table.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SaintFilters } from '../../../saints/interfaces/saint-filter';
import { Saint } from '../../../saints/interfaces/saint';
import { SaintsService } from '../../../../core/services/saints.service';
import { MatDialog } from '@angular/material/dialog';
import { TagsService } from '../../../../core/services/tags.service';
import { ReligiousOrdersService } from '../../../../core/services/religious-orders.service';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { EntityManagerDialogComponent } from '../../../../shared/components/entity-manager-dialog/entity-manager-dialog.component';

@Component({
  selector: 'app-manage-saints-page',
  standalone: true,
  imports: [
    AdminContentTableComponent,
    MatIconModule,
    MatButtonModule,
    RouterLink,
    MatButtonModule,
  ],
  templateUrl: './manage-saints-page.component.html',
  styleUrls: ['./manage-saints-page.component.scss'],
})
export class ManageSaintsPageComponent {
  private saintsService = inject(SaintsService);
  private tagsService = inject(TagsService);
  private religiousOrdersService = inject(ReligiousOrdersService);
  readonly dialog = inject(MatDialog);
  saintsFilter: SaintFilters = new SaintFilters();
  entityFilter: EntityFilters = new EntityFilters();

  saints: Saint[] = [];

  ngOnInit(): void {
    this.loadSaints();
  }

  loadSaints = () => {
    this.saintsService.getSaints(this.saintsFilter).subscribe((res) => {
      this.saints = res.items;
    });
  };

  deleteSaint = (id: number) => this.saintsService.deleteSaint(id);

  manageTags() {
    this.dialog.open(EntityManagerDialogComponent, {
      height: '600px',
      panelClass: 'entity-manager-dialog',
      data: {
        entityName: 'Tag',
        getAllFn: (filters: EntityFilters) => this.tagsService.getTags(filters),
        createFn: (name: string) =>
          this.tagsService.createTag(name, TagType.Saint),
        updateFn: (entity: any) => this.tagsService.updateTag(entity),
        deleteFn: (id: number) => this.tagsService.deleteTag(id),
      },
    });
  }

  manageReligiousOrders() {
    this.dialog.open(EntityManagerDialogComponent, {
      height: '600px',
      panelClass: 'entity-manager-dialog',
      data: {
        entityName: 'Religious Order',
        getAllFn: (filters: EntityFilters) =>
          this.religiousOrdersService.getOrders(filters),
        createFn: (name: string) =>
          this.religiousOrdersService.createOrder(name),
        updateFn: (entity: any) =>
          this.religiousOrdersService.updateOrder(entity),
        deleteFn: (id: number) => this.religiousOrdersService.deleteOrder(id),
      },
    });
  }
}
