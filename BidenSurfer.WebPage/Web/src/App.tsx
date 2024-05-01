import React from 'react';
import { ConfigProvider } from 'antd';
import { HelmetProvider } from 'react-helmet-async';
import enUS from 'antd/lib/locale/en_US';
import GlobalStyle from './styles/GlobalStyle';
import 'typeface-montserrat';
import 'typeface-lato';
import { AppRouter } from './components/router/AppRouter';
import { useAppSelector } from './hooks/reduxHooks';
import { themeObject } from './styles/themes/themeVariables';

const App: React.FC = () => {
  const theme = useAppSelector((state) => state.theme.theme);
  return (
    <>
      <meta name="theme-color" content={themeObject[theme].primary} />
      <GlobalStyle />
      <HelmetProvider>
        <ConfigProvider locale={enUS}>
          <AppRouter />
        </ConfigProvider>
      </HelmetProvider>
    </>
  );
};

export default App;
