package com.example.kotlin_sample

import android.content.Context

class AppPreferences(context: Context) {
  private val preferences =
    context.applicationContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

  var appId: String
    get() = preferences.getString(KEY_APP_ID, "") ?: ""
    set(value) {
      preferences.edit().putString(KEY_APP_ID, value).apply()
    }

  companion object {
    private const val PREFS_NAME = "tmm_sample_prefs"
    private const val KEY_APP_ID = "SampleAppID"
  }
}
