import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { PreventUnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { ListResolver } from './_resolvers/list.resolver';
import { MessagesResolver } from './_resolvers/messages.resolver';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent },
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children:
        [
            { path: 'matches', component: MemberListComponent, resolve: { users: MemberListResolver} },
            { path: 'matches/:id', component: MemberDetailComponent, resolve: { user : MemberDetailResolver} },
            { path: 'profile/edit', component: MemberEditComponent,
              resolve: { user : MemberEditResolver}, canDeactivate: [PreventUnsavedChangesGuard] },
            { path: 'lists', component: ListsComponent, resolve: { users: ListResolver} },
            { path: 'messages', component: MessagesComponent, resolve: { messages: MessagesResolver} },
        ]

    },
    { path: '**', redirectTo: '', pathMatch: 'full' }
];
