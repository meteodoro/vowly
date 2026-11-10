import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

// Tela provisória, substituída uma a uma nas próximas tarefas
@Component({
  selector: 'app-placeholder',
  templateUrl: './placeholder.page.html',
})
export class PlaceholderPage {
  protected readonly titulo = inject(ActivatedRoute).snapshot.data['titulo'] as string;
}
