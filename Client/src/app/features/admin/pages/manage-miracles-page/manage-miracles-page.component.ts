import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminContentTableComponent } from '../../../../shared/components/admin-content-table/admin-content-table.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MiracleFilters } from '../../../miracles/interfaces/miracle-filter';
import { Miracle } from '../../../miracles/interfaces/miracle';
import { MiraclesService } from '../../../../core/services/miracles.service';
import { MatDialog } from '@angular/material/dialog';
import { TagsService } from '../../../../core/services/tags.service';
import { EntityFilters, TagType } from '../../../../interfaces/entity-filters';
import { EntityManagerDialogComponent } from '../../../../shared/components/entity-manager-dialog/entity-manager-dialog.component';

@Component({
  selector: 'app-manage-miracles-page',
  standalone: true,
  imports: [
    AdminContentTableComponent,
    MatIconModule,
    MatButtonModule,
    RouterLink,
  ],
  templateUrl: './manage-miracles-page.component.html',
  styleUrls: ['./manage-miracles-page.component.scss'],
})
export class ManageMiraclesPageComponent {
  private miraclesService = inject(MiraclesService);
  readonly dialog = inject(MatDialog);
  private tagsService = inject(TagsService);

  miraclesFilter: MiracleFilters = new MiracleFilters();

  miracles: Miracle[] = [];

  ngOnInit(): void {
    this.loadMiracles();
  }

  loadMiracles = () => {
    this.miraclesService.getMiracles(this.miraclesFilter).subscribe((res) => {
      this.miracles = res.items;
    });
  };

  deleteMiracle = (id: number) => this.miraclesService.deleteMiracle(id);

  manageTags() {
    this.dialog.open(EntityManagerDialogComponent, {
      height: '600px',
      panelClass: 'entity-manager-dialog',
      data: {
        entityName: 'Tag',
        getAllFn: (filters: EntityFilters) => this.tagsService.getTags(filters),
        createFn: (name: string) =>
          this.tagsService.createTag(name, TagType.Miracle),
        updateFn: (entity: any) => this.tagsService.updateTag(entity),
        deleteFn: (id: number) => this.tagsService.deleteTag(id),
      },
    });
  }
}
