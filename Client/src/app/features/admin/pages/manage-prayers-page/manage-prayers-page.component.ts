import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminContentTableComponent } from '../../../../shared/components/admin-content-table/admin-content-table.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';

import { PrayerFilters } from '../../../prayers/interfaces/prayer-filter';
import { Prayer } from '../../../prayers/interfaces/prayer';
import { PrayersService } from '../../../../core/services/prayers.service';
import { TagsService } from '../../../../core/services/tags.service';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { EntityManagerDialogComponent } from '../../../../shared/components/entity-manager-dialog/entity-manager-dialog.component';

@Component({
  selector: 'app-manage-prayers-page',
  standalone: true,
  imports: [
    AdminContentTableComponent,
    MatIconModule,
    MatButtonModule,
    RouterLink,
  ],
  templateUrl: './manage-prayers-page.component.html',
  styleUrls: ['./manage-prayers-page.component.scss'],
})
export class ManagePrayersPageComponent {
  private prayersService = inject(PrayersService);
  readonly dialog = inject(MatDialog);
  private tagsService = inject(TagsService);

  prayersFilter: PrayerFilters = new PrayerFilters();

  prayers: Prayer[] = [];

  ngOnInit(): void {
    this.loadPrayers();
  }

  loadPrayers = () => {
    this.prayersService.getPrayers(this.prayersFilter).subscribe((res) => {
      this.prayers = res.items;
    });
  };

  deletePrayer = (id: number) => this.prayersService.deletePrayer(id);

  manageTags() {
    this.dialog.open(EntityManagerDialogComponent, {
      height: '600px',
      panelClass: 'entity-manager-dialog',
      data: {
        entityName: 'Tag',
        getAllFn: (filters: EntityFilters) =>
          this.tagsService.getTags(
            new EntityFilters({ ...filters, type: TagType.Prayer })
          ),
        createFn: (name: string) =>
          this.tagsService.createTag(name, TagType.Prayer),
        updateFn: (entity: any) => this.tagsService.updateTag(entity),
        deleteFn: (id: number) => this.tagsService.deleteTag(id),
      },
    });
  }
}
