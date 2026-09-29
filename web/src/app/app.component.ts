import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'receply-dashboard';

  // Injected (not just provided in root) so the theme is read and the data-theme attribute set
  // as early as possible, before the first meaningful paint.
  constructor(private readonly theme: ThemeService) {}
}
