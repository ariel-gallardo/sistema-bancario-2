import dayjs from 'dayjs';
import 'dayjs/locale/es';

dayjs.locale('es');

export const formatCurrency = (amount: number, currency: string) =>
  new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency,
    minimumFractionDigits: 2,
  }).format(amount);

export const formatDate = (value: string, format = 'DD MMM, HH:mm[h]') => dayjs(value).format(format);
