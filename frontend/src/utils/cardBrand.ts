import visaLogo from '../assets/visa.svg';
import mastercardLogo from '../assets/mastercard.svg';
import amexLogo from '../assets/amex.svg';

export interface CardBrandMeta {
  displayName: string;
  logo: string;
  tone: string;
  background: string;
}

const brandCatalog: Array<{ match: RegExp; meta: CardBrandMeta }> = [
  {
    match: /visa/i,
    meta: {
      displayName: 'Visa',
      logo: visaLogo,
      tone: '#37b7c3',
      background: 'linear-gradient(120deg, rgba(55, 183, 195, 0.15), rgba(10, 27, 77, 0.7))',
    },
  },
  {
    match: /mastercard/i,
    meta: {
      displayName: 'Mastercard',
      logo: mastercardLogo,
      tone: '#ffb547',
      background: 'linear-gradient(120deg, rgba(255, 181, 71, 0.15), rgba(26, 15, 31, 0.7))',
    },
  },
  {
    match: /(amex|american\s+express)/i,
    meta: {
      displayName: 'American Express',
      logo: amexLogo,
      tone: '#7fd0ff',
      background: 'linear-gradient(120deg, rgba(127, 208, 255, 0.2), rgba(27, 75, 155, 0.65))',
    },
  },
];

const defaultMeta: CardBrandMeta = {
  displayName: 'Tarjeta',
  logo: visaLogo,
  tone: '#f4f6fb',
  background: 'rgba(255, 255, 255, 0.06)',
};

export const resolveCardBrand = (marca?: string | null): CardBrandMeta => {
  if (!marca) {
    return defaultMeta;
  }

  const found = brandCatalog.find((entry) => entry.match.test(marca));
  return found ? found.meta : { ...defaultMeta, displayName: marca };
};
