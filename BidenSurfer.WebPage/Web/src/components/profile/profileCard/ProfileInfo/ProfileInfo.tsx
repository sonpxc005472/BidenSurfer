import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { UserModel } from '@app/domain/UserModel';
import * as S from './ProfileInfo.styles';
import { BaseAvatar } from '@app/components/common/BaseAvatar/BaseAvatar';
import Avatar from '@app/assets/images/avatar5.webp';

interface ProfileInfoProps {
  profileData: UserModel | null;
}

export const ProfileInfo: React.FC<ProfileInfoProps> = ({ profileData }) => {
  const [fullness] = useState(90);

  const { t } = useTranslation();

  return profileData ? (
    <S.Wrapper>
      <S.ImgWrapper>
        <BaseAvatar shape="circle" src={Avatar} alt="Profile" />
      </S.ImgWrapper>
      <S.Title>{`${profileData?.fullName}`}</S.Title>
    </S.Wrapper>
  ) : null;
};
