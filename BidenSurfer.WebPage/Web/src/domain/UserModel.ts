export interface UserModel {
  id: string;
  fullName?: string;
  imgUrl?: string;
  userName: string;
  email: string;
  role: number;
  sex?: 'male' | 'female';
  birthday?: string;
  lang?: 'en' | 'de';
  country?: string;
  city?: string;
  address1?: string;
  address2?: string;
  zipcode?: number;
  website?: string;
  socials?: {
    twitter?: string;
    facebook?: string;
    linkedin?: string;
  };
}
