import '@radix-ui/themes/styles.css';
import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import { Theme, ThemePanel } from '@radix-ui/themes';
import { ThemeProvider } from 'next-themes';
import { AuthProvider } from './AuthContext';

createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ThemeProvider attribute="class">
      <Theme appearance="dark" accentColor="green" panelBackground="translucent">
        <AuthProvider>
          <App />
        </AuthProvider>
      </Theme>
    </ThemeProvider>
  </React.StrictMode>,
);
