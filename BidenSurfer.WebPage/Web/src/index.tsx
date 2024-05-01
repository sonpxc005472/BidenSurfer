import App from './App';
import './i18n';
import 'config/config';
import { Provider } from 'react-redux';
import { store } from '@app/store/store';
import { createRoot } from 'react-dom/client';
import React from 'react';


const container = document.getElementById('root') as HTMLElement;
const root = createRoot(container);

root.render(
  <Provider store={store}>
    <App />
  </Provider>
);
