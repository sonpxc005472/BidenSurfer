import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { SearchDropdown } from '../searchDropdown/SearchDropdown';
import { BaseButton } from '@app/components/common/BaseButton/BaseButton';
import { components as configComponents, Component } from '@app/constants/config/components';
import { categoriesList, CategoryType } from '@app/constants/categoriesList';
import { useResponsive } from '@app/hooks/useResponsive';
import * as S from './HeaderSearch.styles';

export interface CategoryComponents {
  category: CategoryType;
  components: Component[];
}

export const HeaderSearch: React.FC = () => {
  const { isTablet } = useResponsive();

  const { pathname } = useLocation();

  const [query, setQuery] = useState('');
  const [components] = useState<Component[]>(configComponents);

  const [isOverlayOpen, setOverlayOpen] = useState(false);

  const sortedResults = query
    ? categoriesList.reduce((acc, current) => {
        const searchResults = components.filter(
          (component) =>
            component.categories.includes(current.name) &&
            component.keywords.some((keyword) => keyword.includes(query)),
        );

        return searchResults.length > 0 ? acc.concat({ category: current.name, components: searchResults }) : acc;
      }, [] as CategoryComponents[])
    : null;

  useEffect(() => {
    setOverlayOpen(false);
  }, [pathname]);

  return (
    <>
      {isTablet && (
        <SearchDropdown
          query={query}
          setQuery={setQuery}
          data={sortedResults}
          isOverlayOpen={isOverlayOpen}
          setOverlayOpen={setOverlayOpen}
        />
      )}
    </>
  );
};
