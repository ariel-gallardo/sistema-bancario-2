import { Box, Button, LinearProgress, Paper, Stack, Typography } from '@mui/material';
import CreditScoreRoundedIcon from '@mui/icons-material/CreditScoreRounded';
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded';
import type { ResumenClienteResponse } from '../types/api';

interface AccountCardProps {
  isLoading: boolean;
  resumen?: ResumenClienteResponse;
  clienteId?: number | null;
  onRefresh?: () => void;
  formatCurrency: (value: number, currency: string) => string;
}

const AccountCard = ({ isLoading, resumen, clienteId, onRefresh, formatCurrency }: AccountCardProps) => (
  <Paper elevation={0} className="account-paper">
    <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
      <Stack spacing={0.5}>
        <Typography variant="h5">Saldo cuenta principal</Typography>
        <Typography variant="body2" color="text.secondary">
          {clienteId ? `Cliente ${clienteId}` : 'Ingresá un cliente para consultar el saldo'}
        </Typography>
      </Stack>
      <Stack direction="row" spacing={1} alignItems="center">
        {onRefresh && (
          <Button
            size="small"
            variant="outlined"
            startIcon={<RefreshRoundedIcon />}
            onClick={onRefresh}
            disabled={isLoading || !clienteId}
          >
            Actualizar
          </Button>
        )}
        <CreditScoreRoundedIcon fontSize="large" color="primary" />
      </Stack>
    </Stack>

    <Box mt={3}>
      {isLoading ? (
        <LinearProgress />
      ) : resumen ? (
        <Stack spacing={2}>
          <Typography variant="h2" className="saldo-principal">
            {formatCurrency(resumen.saldoCuentaPrincipal, 'ARS')}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {resumen.tieneTarjetaPrincipal
              ? 'La cuenta está asociada a una tarjeta principal activa.'
              : 'Este cliente aún no posee una tarjeta principal asignada.'}
          </Typography>
        </Stack>
      ) : (
        <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
          Todavía no consultamos datos. Ingresá un cliente para iniciar la búsqueda.
        </Typography>
      )}
    </Box>
  </Paper>
);

export default AccountCard;
