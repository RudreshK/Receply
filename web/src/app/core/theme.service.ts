import { Injectable, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'receply.theme';

/** App-wide light/dark theme, driven by a `data-theme` attribute on <html> that every component's
 * CSS custom properties (see styles.scss) key off. Instantiated once in AppComponent so the
 * attribute is set before the first paint. */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<ThemeMode>(this.readInitial());

  constructor() {
    effect(() => this.applyTheme(this.theme()));
  }

  toggle(): void {
    this.theme.set(this.theme() === 'dark' ? 'light' : 'dark');
  }

  private applyTheme(mode: ThemeMode): void {
    document.documentElement.setAttribute('data-theme', mode);
    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      // Storage can be unavailable (private mode) - the theme still applies for this session.
    }
  }

  private readInitial(): ThemeMode {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored === 'light' || stored === 'dark') return stored;
    } catch {
      // Fall through to the OS preference.
    }
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
