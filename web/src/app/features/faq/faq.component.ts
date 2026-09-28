import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Faq } from '../../core/models';

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './faq.component.html',
  styleUrl: './faq.component.scss'
})
export class FaqComponent implements OnInit {
  faqs = signal<Faq[]>([]);
  loading = signal(true);

  newQuestion = '';
  newAnswer = '';

  editingId = signal<string | null>(null);
  editQuestion = '';
  editAnswer = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.api.listFaqs().subscribe({
      next: (faqs) => {
        this.faqs.set(faqs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  add(): void {
    if (!this.newQuestion.trim() || !this.newAnswer.trim()) {
      return;
    }
    this.api.createFaq({ question: this.newQuestion.trim(), answer: this.newAnswer.trim() }).subscribe(() => {
      this.newQuestion = '';
      this.newAnswer = '';
      this.load();
    });
  }

  startEdit(faq: Faq): void {
    this.editingId.set(faq.id);
    this.editQuestion = faq.question;
    this.editAnswer = faq.answer;
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(faq: Faq): void {
    this.api
      .updateFaq(faq.id, { question: this.editQuestion.trim(), answer: this.editAnswer.trim(), isActive: faq.isActive })
      .subscribe(() => {
        this.editingId.set(null);
        this.load();
      });
  }

  toggleActive(faq: Faq): void {
    this.api.updateFaq(faq.id, { question: faq.question, answer: faq.answer, isActive: !faq.isActive }).subscribe(() => this.load());
  }

  remove(id: string): void {
    this.api.deleteFaq(id).subscribe(() => this.load());
  }
}
