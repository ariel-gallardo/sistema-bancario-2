import { Alert, Avatar, Divider, LinearProgress, List, ListItem, ListItemAvatar, ListItemText, Paper, Stack, Typography } from '@mui/material';
import type { MovimientoResponse, TarjetaResponse } from '../types/api';

interface MovementsCardProps {
  isLoading: boolean;
  movimientos: MovimientoResponse[];
  tarjeta?: TarjetaResponse | null;
  error?: string;
  formatCurrency: (value: number, currency: string) => string;
  formatDate: (value: string, format?: string) => string;
}

const MovementsCard = ({ isLoading, movimientos, tarjeta, error, formatCurrency, formatDate }: MovementsCardProps) => (
  <Paper elevation={0} className="movements-paper">
    <Stack direction="row" justifyContent="space-between" alignItems="center" className="movements-header">
      <Typography variant="h4" className="movements-title">
        Últimos consumos
      </Typography>
      {tarjeta && (
        <Typography variant="body2" color="text.secondary">
          Tarjeta {tarjeta.numero}
        </Typography>
      )}
    </Stack>
    <Divider sx={{ my: 2.5, opacity: 0.2 }} />
    {isLoading && <LinearProgress />}

    {!isLoading && tarjeta === null && (
      <Alert severity="info" sx={{ my: 3 }}>
        Este cliente no tiene tarjeta principal, por eso no hay movimientos para mostrar.
      </Alert>
    )}

    {!isLoading && tarjeta && movimientos.length === 0 && (
      <Alert severity="info" sx={{ my: 3 }}>
        No encontramos movimientos recientes para la tarjeta seleccionada.
      </Alert>
    )}

    {!isLoading && tarjeta && movimientos.length > 0 && (
      <List>
        {movimientos.map((movimiento) => (
          <ListItem key={movimiento.id} disableGutters className="movement-item-row">
            <ListItemAvatar>
              <Avatar>{movimiento.descripcion[0]}</Avatar>
            </ListItemAvatar>
            <ListItemText
              primary={
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography variant="h6">{movimiento.descripcion}</Typography>
                  <Typography variant="h6">{formatCurrency(movimiento.monto, 'ARS')}</Typography>
                </Stack>
              }
              secondary={
                <Stack direction="row" justifyContent="space-between">
                  <Typography variant="body2" color="text.secondary">
                    Tarjeta #{tarjeta.id}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {formatDate(movimiento.fecha, 'DD MMM YYYY')}
                  </Typography>
                </Stack>
              }
            />
          </ListItem>
        ))}
      </List>
    )}

    {error && (
      <Alert severity="warning" sx={{ mt: 2 }}>
        {error}
      </Alert>
    )}
  </Paper>
);

export default MovementsCard;
