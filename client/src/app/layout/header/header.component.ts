import { Component } from '@angular/core';
import { MatBadge } from '@angular/material/badge';
import { MAT_BUTTON_CONFIG, MatButton } from '@angular/material/button';
import {MatIcon} from "@angular/material/icon";

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    MatIcon,
    MatButton,
    MatBadge
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {

}
