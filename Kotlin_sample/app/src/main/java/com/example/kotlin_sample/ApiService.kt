package com.example.kotlin_sample

import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Header

interface ApiService {
  @GET("api/v1/receiver")
  fun getReceiverData(
    @Header("Authorization") authHeader: String
  ): Call<ReceiverResponse>
}

data class ReceiverResponse(
  val data: String // Adjust this to match the actual response structure
)
