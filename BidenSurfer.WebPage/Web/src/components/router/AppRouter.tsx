import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

// no lazy loading for auth pages to avoid flickering
const AuthLayout = React.lazy(() => import('@app/components/layouts/AuthLayout/AuthLayout'));
import LoginPage from '@app/pages/LoginPage';

import MainLayout from '@app/components/layouts/main/MainLayout/MainLayout';
import ProfileLayout from '@app/components/profile/ProfileLayout';
import RequireAuth from '@app/components/router/RequireAuth';
import { withLoading } from '@app/hocs/withLoading.hoc';
import ApiSettingPage from '@app/pages/ApiSettingPage';

const ServerErrorPage = React.lazy(() => import('@app/pages/ServerErrorPage'));
const Error404Page = React.lazy(() => import('@app/pages/Error404Page'));
const AdvancedFormsPage = React.lazy(() => import('@app/pages/AdvancedFormsPage'));
const ConfigurationPage = React.lazy(() => import('@app/pages/ConfigurationPage'));
const SettingPage = React.lazy(() => import('@app/pages/SettingPage'));

const PersonalInfoPage = React.lazy(() => import('@app/pages/PersonalInfoPage'));
const SecuritySettingsPage = React.lazy(() => import('@app/pages/SecuritySettingsPage'));
const NotificationsPage = React.lazy(() => import('@app/pages/NotificationsPage'));
const ButtonsPage = React.lazy(() => import('@app/pages/uiComponentsPages/ButtonsPage'));
const SpinnersPage = React.lazy(() => import('@app/pages/uiComponentsPages/SpinnersPage'));
const AvatarsPage = React.lazy(() => import('@app/pages/uiComponentsPages/dataDisplay/AvatarsPage'));
const BadgesPage = React.lazy(() => import('@app/pages/uiComponentsPages/dataDisplay/BadgesPage'));
const CollapsePage = React.lazy(() => import('@app/pages/uiComponentsPages/dataDisplay/CollapsePage'));
const PaginationPage = React.lazy(() => import('@app/pages/uiComponentsPages/dataDisplay/PaginationPage'));
const ModalsPage = React.lazy(() => import('@app/pages/uiComponentsPages/modals/ModalsPage'));
const PopoversPage = React.lazy(() => import('@app/pages/uiComponentsPages/modals/PopoversPage'));
const PopconfirmsPage = React.lazy(() => import('@app/pages/uiComponentsPages/modals/PopconfirmsPage'));
const ProgressPage = React.lazy(() => import('@app/pages/uiComponentsPages/feedback/ProgressPage'));
const ResultsPage = React.lazy(() => import('@app/pages/uiComponentsPages/feedback/ResultsPage'));
const AlertsPage = React.lazy(() => import('@app/pages/uiComponentsPages/feedback/AlertsPage'));
const SkeletonsPage = React.lazy(() => import('@app/pages/uiComponentsPages/feedback/SkeletonsPage'));
const InputsPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/InputsPage'));
const CheckboxesPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/CheckboxesPage'));
const RadiosPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/RadiosPage'));
const SelectsPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/SelectsPage'));
const SwitchesPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/SwitchesPage'));
const UploadsPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/UploadsPage'));
const RatesPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/RatesPage'));
const AutoCompletesPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/AutoCompletesPage'));
const StepsPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/StepsPage'));
const DateTimePickersPage = React.lazy(() => import('@app/pages/uiComponentsPages/forms/DateTimePickersPage'));
const DropdownsPage = React.lazy(() => import('@app/pages/uiComponentsPages/DropdownsPage'));
const BreadcrumbsPage = React.lazy(() => import('@app/pages/uiComponentsPages/navigation/BreadcrumbsPage'));
const TabsPage = React.lazy(() => import('@app/pages/uiComponentsPages/navigation/TabsPage'));
const NotificationsUIPage = React.lazy(() => import('@app/pages/uiComponentsPages/feedback/NotificationsPage'));
const Logout = React.lazy(() => import('./Logout'));

export const DASHBOARD_PATH = '/';
export const MEDICAL_DASHBOARD_PATH = '/medical-dashboard';

const AdvancedForm = withLoading(AdvancedFormsPage);
const Configurations = withLoading(ConfigurationPage);
const Settings = withLoading(SettingPage);

// UI Components
const Buttons = withLoading(ButtonsPage);
const Spinners = withLoading(SpinnersPage);
const Inputs = withLoading(InputsPage);
const Checkboxes = withLoading(CheckboxesPage);
const Radios = withLoading(RadiosPage);
const Selects = withLoading(SelectsPage);
const Switches = withLoading(SwitchesPage);
const Uploads = withLoading(UploadsPage);
const Rates = withLoading(RatesPage);
const AutoCompletes = withLoading(AutoCompletesPage);
const Steps = withLoading(StepsPage);
const DateTimePickers = withLoading(DateTimePickersPage);
const Dropdowns = withLoading(DropdownsPage);
const Breadcrumbs = withLoading(BreadcrumbsPage);
const Tabs = withLoading(TabsPage);
const Avatars = withLoading(AvatarsPage);
const Badges = withLoading(BadgesPage);
const Collapse = withLoading(CollapsePage);
const Pagination = withLoading(PaginationPage);
const Modals = withLoading(ModalsPage);
const Popovers = withLoading(PopoversPage);
const Popconfirms = withLoading(PopconfirmsPage);
const Progress = withLoading(ProgressPage);
const Results = withLoading(ResultsPage);
const Alerts = withLoading(AlertsPage);
const NotificationsUI = withLoading(NotificationsUIPage);
const Skeletons = withLoading(SkeletonsPage);

const ServerError = withLoading(ServerErrorPage);
const Error404 = withLoading(Error404Page);

// Profile
const PersonalInfo = withLoading(PersonalInfoPage);
const SecuritySettings = withLoading(SecuritySettingsPage);
const Notifications = withLoading(NotificationsPage);

const AuthLayoutFallback = withLoading(AuthLayout);
const LogoutFallback = withLoading(Logout);

export const AppRouter: React.FC = () => {
  const protectedLayout = (
    <RequireAuth>
      <MainLayout />
    </RequireAuth>
  );

  return (
    <BrowserRouter>
      <Routes>
        <Route path={DASHBOARD_PATH} element={protectedLayout}>          
          <Route path="apps" />        
          <Route path="configurations" element={<Configurations />} />    
          <Route path="api-settings" element={<ApiSettingPage />} />                
          <Route path="server-error" element={<ServerError />} />
          <Route path="404" element={<Error404 />} />
          <Route path="profile" element={<ProfileLayout />}>
            <Route path="personal-info" element={<PersonalInfo />} />
            <Route path="security-settings" element={<SecuritySettings />} />
            <Route path="notifications" element={<Notifications />} />
          </Route>          
        </Route>
        <Route path="/login" element={<AuthLayoutFallback />}>
          <Route path="" element={<LoginPage />} />          
        </Route>
        <Route path="/logout" element={<LogoutFallback />} />
      </Routes>
    </BrowserRouter>
  );
};
