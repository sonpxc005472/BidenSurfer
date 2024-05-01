import React from 'react';
import { UserModel } from '@app/domain/UserModel';
import * as S from './ProfileInfo.styles';
import { BaseAvatar } from '@app/components/common/BaseAvatar/BaseAvatar';
import Avatar from '@app/assets/images/avatar5.webp';

interface ProfileInfoProps {
  profileData: UserModel | null;
}

export const ProfileInfo: React.FC<ProfileInfoProps> = ({ profileData }) => {
  return profileData ? (
    <S.Wrapper>
      <S.ImgWrapper>
        <BaseAvatar shape="circle" src={Avatar} alt="Profile" />
      </S.ImgWrapper>
      <S.Title>{`${profileData?.username}`}</S.Title>
    </S.Wrapper>
  ) : null;
};
