import android.content.Context
import andoird.content.Intent
import android.net.Uri
import androidx.activity.result.contract.ActivityResultContract

class CustomActivityResultContract : ActivityResultContract<Uri, Uri?>()
{
  override fun createIntent(context: Context, input: Uri): Intent {
    return Intent(Intent.ACTION_VIEW, input)
  }

  override fun parseResult(resultCode: Int, intent: Intent?): Uri? {
    return intent?.data
  }
}
